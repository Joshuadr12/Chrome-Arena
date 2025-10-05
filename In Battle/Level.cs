using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Chrome Arena/Level")]
public class Level : ScriptableObject
{
    /// <summary>
    /// Represents a level or battle.
    /// </summary>

    // Variables.
    public string levelId;
    [FormerlySerializedAs("name")] public string levelName;
    public Unit opponentDisplay;
    public RequirementSet requirements;
    [Header("Difficulty and Stars")]
    [Tooltip("The amount of money to assign to each squad, including the must-haves and the extras")] public int squadSize;
    [Tooltip("The maximum player squad size for the Unfair Star")] public int unfairSquadSize;
    public StarChallenges starChallenges;
    [Header("Squads")]
    [Tooltip("The enemy squads to be used necessarily")] public List<Squad> mustHaves;
    [Tooltip("The enemy squads to be chosen at random after the must-haves")] public List<Squad> extraSquads;
    [Header("Miscellaneous")]
    public BattleSettings settings;

    // TODO: Replace constant 3 after adding rounds variable
    public List<Squad> ChooseSquads()
    {
        /// <summary>Decides and returns a list of squads to use in battle.</summary>

        List<Squad> chosen = new List<Squad>();
        int squadIndex = 0;
        if (mustHaves.Count + extraSquads.Count < 3)
        {
            Debug.LogError("Total squad count needs to equal or exceed the number of rounds.");
            return chosen;
        }

        // Add the must-haves.
        foreach (Squad must in mustHaves)
        {
            if (chosen.Count < 3)
            {
                must.money = squadSize;
                chosen.Add(must);
                squadIndex++;
            }
        }

        // Choose and add the extras.
        Squad squad;
        int moneyIndex;
        while (chosen.Count < 3)
        {
            squadIndex = UnityEngine.Random.Range(0, extraSquads.Count);
            squad = extraSquads[squadIndex];
            if (!chosen.Contains(squad))
            {
                moneyIndex = squadIndex + mustHaves.Count;
                squad.money = squadSize;
                chosen.Add(extraSquads[squadIndex]);
            }
        }
        return chosen;
    }
}

[Serializable]
public class BattleSettings
{
    /// <summary>
    /// Stores miscellaneous level/battle settings.
    /// </summary>

    // Variables.
    public AudioClip specialMusic;
    public bool allowArtifacts = true;
    //public int rounds = 3;
    public int lanes = 5;
    [Tooltip("The maximum number of fighters in each lane")] public int laneCapacity = 5;
}

[Serializable]
public class StarChallenges
{
    /// <summary>
    /// Keeps track of star challenges and when they are completed.
    /// </summary>

    public static ScoreSet totalScore = new ScoreSet(),
        tempScore = new ScoreSet();

    public List<Challenge> challenges;

    public static void AddTempScore()
    {
        totalScore.roundsWon += tempScore.roundsWon;
        totalScore.casualties += tempScore.casualties;
        totalScore.attacks += tempScore.attacks;
        totalScore.damageDealt += tempScore.damageDealt;
        totalScore.criticals += tempScore.criticals;
        totalScore.abilityTriggers += tempScore.abilityTriggers;
        totalScore.blocks += tempScore.blocks;
        totalScore.colorGain += tempScore.colorGain;
    }

    public class ScoreSet
    {
        public int roundsWon, casualties, attacks,
            damageDealt, criticals, abilityTriggers,
            blocks, colorGain;

        public void Reset()
        {
            roundsWon = 0;
            casualties = 0;
            attacks = 0;
            damageDealt = 0;
            criticals = 0;
            abilityTriggers = 0;
            blocks = 0;
            colorGain = 0;
        }
    }

    [Serializable]
    public class Challenge
    {
        public enum ChallengeType
        {
            WinRounds,
            MaxCasualties,
            MaxAttacks,
            DamageDealt,
            CriticalHits,
            AbilityTriggers,
            Blocks,
            ColorGain
        }

        public ChallengeType challenge;
        public int scoreNeeded;

        public bool IsCompleted(ScoreSet score)
        {
            switch (challenge)
            {
                case ChallengeType.WinRounds:
                    // TODO: Replace 3 with the number of squads when it begins to vary.
                    return score.roundsWon >=
                        (scoreNeeded <= 0
                        ? 3
                        : scoreNeeded);
                case ChallengeType.MaxCasualties:
                    return score.casualties <= scoreNeeded;
                case ChallengeType.MaxAttacks:
                    return score.attacks <= scoreNeeded;
                case ChallengeType.DamageDealt:
                    return score.damageDealt >= scoreNeeded;
                case ChallengeType.CriticalHits:
                    return score.criticals >= scoreNeeded;
                case ChallengeType.AbilityTriggers:
                    return score.abilityTriggers >= scoreNeeded;
                case ChallengeType.Blocks:
                    return score.blocks >= scoreNeeded;
                case ChallengeType.ColorGain:
                    return score.colorGain >= scoreNeeded;
                default:
                    Debug.LogWarning($"Unknown challenge type: {challenge}.");
                    return false;
            }
        }

        public string GetDescription(bool displayProgress = false)
        {
            string temp = "";
            switch (challenge)
            {
                case ChallengeType.WinRounds:
                    // TODO: Replace 3 with the number of squads when it begins to vary.
                    return scoreNeeded <= 0
                        ? "Win Every Round"
                        : $"Win {scoreNeeded} Rounds";
                case ChallengeType.MaxCasualties:
                    if (displayProgress)
                        temp += $"{totalScore.casualties}/";
                    return $"{temp}{scoreNeeded} MAX Casualties";
                case ChallengeType.MaxAttacks:
                    if (displayProgress)
                        temp += $"{totalScore.attacks}/";
                    return $"{temp}{scoreNeeded} MAX Attack Phases";
                case ChallengeType.DamageDealt:
                    if (displayProgress)
                        temp += $"{totalScore.damageDealt}/";
                    return $"{temp}{scoreNeeded} Damage Dealt";
                case ChallengeType.CriticalHits:
                    if (displayProgress)
                        temp += $"{totalScore.criticals}/";
                    return $"{temp}{scoreNeeded} Critical Hits";
                case ChallengeType.AbilityTriggers:
                    if (displayProgress)
                        temp += $"{totalScore.abilityTriggers}/";
                    return $"{temp}{scoreNeeded} Ability Triggers";
                case ChallengeType.Blocks:
                    if (displayProgress)
                        temp += $"{totalScore.blocks}/";
                    return $"{temp}{scoreNeeded} Blocks";
                case ChallengeType.ColorGain:
                    if (displayProgress)
                        temp += $"{totalScore.colorGain}/";
                    return $"{temp}{scoreNeeded} Color Gained from Abilities";
                default:
                    Debug.LogWarning($"Unknown challenge type: {challenge}.");
                    return scoreNeeded.ToString();
            }
        }
    }
}