using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using TMPro;

public class SquadMenu : MonoBehaviour
{
    /// <summary>
    /// Manages the squad menu when choosing squads for a battle.
    /// </summary>

    // Serialized variables for the editor.
    [SerializeField] List<Image> starHeader;
    [SerializeField] List<TMP_Dropdown> squadChoices;
    [SerializeField, Tooltip("The parent object of the squad size settings.")] Transform squadSizeList;
    [SerializeField, Tooltip("Do not include the first star; it is always FREE.")] List<TMP_Text> starRequirements;
    [SerializeField] Button goButton;
    [SerializeField] TMP_Text levelTitle;
    [SerializeField] List<UnitDisplay> enemyImages;
    [SerializeField] List<Image> artifactImages;
    [SerializeField] GameObject dialoguePanel;

    // Miscellaneous variables.
    int index;
    SortingGroup displaySprite;
    List<int> squadAmounts = new List<int>();
    List<string> squadsChosen = new List<string>(),
        allSquads = new List<string>();
    List<Unit> unitsFound = new List<Unit>();
    List<Artifact> artifactsFound = new List<Artifact>();

    public void SetupMenu()
    {
        /// <summary>Generate the squad menu in accordance with the selected level.</summary>

        // Configure the squad dropdowns.
        allSquads.Clear();
        foreach (Squad squad in Master.data.squads)
            allSquads.Add(squad.squadName);
        squadsChosen.Clear();
        foreach (int n in Master.data.defaultSquads)
            squadsChosen.Add(allSquads[n]);
        RenderSquads();

        // Display the enemy units.
        levelTitle.text = Master.levelSelected.levelName;
        unitsFound.Clear();
        FindEnemies(Master.levelSelected.mustHaves);
        FindEnemies(Master.levelSelected.extraSquads);
        index = 0;
        foreach (List<Unit> collection in Master.unitSets.Values)
            foreach (Unit unit in collection)
            {
                if
                    (unitsFound.Contains(unit)
                    && (index < enemyImages.Count))
                {
                    enemyImages[index].ChangeUnit(unit, "neutral");
                    enemyImages[index].gameObject.SetActive(true);
                    displaySprite = enemyImages[index]
                        .animator
                        .GetComponent<SortingGroup>();
                    displaySprite.sortingOrder += index;
                    index++;
                }
            }
        while (index < enemyImages.Count)
        {
            enemyImages[index].gameObject.SetActive(false);
            index++;
        }

        // Display the enemy artifacts.
        artifactsFound.Clear();
        FindArtifacts(Master.levelSelected.mustHaves);
        FindArtifacts(Master.levelSelected.extraSquads);
        index = 0;
        foreach (Artifact artifact in Master.artifacts)
            if
                (artifactsFound.Contains(artifact)
                && (index < artifactImages.Count))
            {
                artifactImages[index].gameObject.SetActive(true);
                artifactImages[index].sprite = artifact.sprite;
                index++;
            }
        while (index < artifactImages.Count)
        {
            artifactImages[index].gameObject.SetActive(false);
            index++;
        }

        // Display the star count and requirements.
        for (int i = 0; i < starHeader.Count; i++)
            starHeader[i].color = i < Master.GetStars()
                ? Master.goldColor
                : Color.black;
        starRequirements[0].text = $"FAIR: {Master.levelSelected.squadSize} squad size";
        starRequirements[1].text = $"UNFAIR: {Master.levelSelected.unfairSquadSize} squad size";
        starRequirements[2].text = Master
            .levelSelected
            .starChallenges
            .challenges[0]
            .GetDescription();
        starRequirements[3].text = Master
            .levelSelected
            .starChallenges
            .challenges[1]
            .GetDescription();

        dialoguePanel.SetActive(true);
    }

    void RenderSquads()
    {
        Master.RenderSquadDropdowns(squadChoices, squadsChosen, squadAmounts);

        goButton.interactable = true;
        TMP_Text[] squadAmountText = squadSizeList.GetComponentsInChildren<TMP_Text>();
        for (int i = 0; i < squadAmountText.Length; i++)
        {
            squadAmountText[i].text = squadAmounts[i].ToString();
            if (squadAmounts[i] <= 0)
                goButton.interactable = false;
        }
    }

    public void MakeChoice(int choiceIndex)
    {
        /// <summary>Select the given button for the list of chosen squads.</summary>
        /// <param name="buttonIndex">The index of the button being selected.</param>

        TMP_Dropdown dropdown = squadChoices[choiceIndex];
        squadsChosen[choiceIndex] = dropdown.captionText.text;
        RenderSquads();
    }

    public void IncreaseSquadSize(int index)
    {
        squadAmounts[index] += 10;
        RenderSquads();
    }
    public void DecreaseSquadSize(int index)
    {
        if (squadAmounts[index] > 10)
        {
            squadAmounts[index] = Mathf.CeilToInt((float)(squadAmounts[index] - 10) / 10) * 10;
            RenderSquads();
        }
    }

    void FindEnemies(List<Squad> squads)
    {
        foreach (Squad squad in squads)
            foreach (Line line in squad.units)
                foreach (Unit u in line.units)
                    if (!unitsFound.Contains(u))
                        unitsFound.Add(u);
    }
    void FindArtifacts(List<Squad> squads)
    {
        foreach (Squad squad in squads)
            if (!artifactsFound.Contains(squad.artifact))
                artifactsFound.Add(squad.artifact);
    }

    public void Go()
    {
        /// <summary>Initiate the battle.</summary>

        // Add chosen squads.
        Master.leftSquads.Clear();
        foreach (Squad s in Master.data.squads)
            if (squadsChosen.Contains(s.squadName))
                Master.leftSquads.Add(s);

        // Check for fair and unfair stars.
        SquadSelect.fairStars = 2;
        foreach (int amount in squadAmounts)
        {
            if (amount > Master.levelSelected.squadSize
                && SquadSelect.fairStars > 0)
                SquadSelect.fairStars = 0;
            else if (amount > Master.levelSelected.unfairSquadSize
                && SquadSelect.fairStars > 1)
                SquadSelect.fairStars = 1;
        }

        Master.GotoScene("SquadSelect");
    }
}