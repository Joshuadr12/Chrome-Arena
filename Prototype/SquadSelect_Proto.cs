using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadSelect_Proto : SquadSelect
{
    /// <summary>
    /// Manages the squad selection menu in battle for the prototype scene.
    /// </summary>
    
    [Header("Prototype"), SerializeField] Battle_Proto battleProto;
    [SerializeField] GameObject round2Textwall, diagram;

    // Start is called before the first frame update.
    void Start()
    {
        source = GetComponent<AudioSource>();
        source.volume = Master.data.sfxVolume;

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
        ChooseRightSquad();
    }

    // Update is called once per frame.
    void Update()
    {
        if (!Battle.inBattle)
        {
            // Leave the battle.
            if (Input.GetKeyDown(KeyCode.Escape))
                StartCoroutine(Master.GotoScene("Prototype", false));
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

    new public void NextRound()
    {
        Master.OpenMenu(gameObject, battleMenu);

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

            resultsText.gameObject.SetActive(true);
            resultsText.text = totalOutcome > 0
                ? winMessage : loseMessage;
        }
        else
        {
            leftChoice = null;
            ChooseRightSquad();
            if (roundsDone == 1)
            {
                round2Textwall.SetActive(true);
                diagram.SetActive(true);
            }
        }
    }

    new public void SetChoice(int squadIndex)
    {
        /// <summary>Set the player's choice and proceed to battle.</summary>
        /// <param name="squadIndex">The index of the choice being made.</param>

        leftChoice = leftSquads[squadIndex];
        if (!Master.FinishedTutorial()
            && (roundsDone <= 1))
            rightChoice = FindPreciseMatch(roundsDone == 1);
        else if (!Master.FinishedTutorial("basic_2"))
            rightChoice = FindEasiestMatch();

        Battle.leftSide = leftChoice.squad;
        Battle.rightSide = rightChoice.squad;

        float advantage = Master
            .colours[Battle.leftSide.colour]
            .Advantage(Master.colours[Battle.rightSide.colour]);
        if (advantage != 0)
            BattleScroll_Proto.main.Broadcast(advantage, true);

        Master.OpenMenu(battleMenu, gameObject);
        battleProto.StartBattle();
    }
}