using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DialogueScene : MonoBehaviour
{
    public const float offscreenPos = 13;

    [SerializeField] Image panel;
    [SerializeField] List<Fighter> actors;
    [SerializeField] Unit defaultUnit;
    [SerializeField] GameObject textBox, choiceBox;
    [SerializeField] TMP_Text header, body, choice1, choice2;
    [SerializeField] Image image;
    [SerializeField] List<DialogueEvent> events;

    bool isDone = false;
    float spriteIndex = 0;
    int choice;
    Color panelColor;
    Fighter actor;
    SortingGroup displaySprite;
    DialogueEvent.Dialogue dialogue;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        panelColor = panel.color;
        panel.color = Color.clear;

        actors[0].transform.position = Vector3.left * offscreenPos;
        actors[1].transform.position = Vector3.left * offscreenPos;
        actors[2].transform.position = Vector3.right * offscreenPos;
        actors[3].transform.position = Vector3.right * offscreenPos;
        foreach (Fighter actor in actors)
        {
            actor.ChangeUnit(defaultUnit);
            actor.NewPos(actor.transform.position);
            displaySprite = actor
                .GetComponent<UnitDisplay>()
                .animator
                .GetComponent<SortingGroup>();
            displaySprite.sortingOrder = 10;
        }

        StartCoroutine(ExecuteScenes());
    }

    // Update is called once per frame
    void Update()
    {
        if ((dialogue != null) && (dialogue.imageSprite.Count > 0))
        {
            spriteIndex = (spriteIndex + Time.deltaTime * 30)
                % dialogue.imageSprite.Count;
            image.sprite = dialogue.imageSprite[Mathf.FloorToInt(spriteIndex)];
        }

        if (isDone)
            gameObject.SetActive(false);
    }

    public void MakeChoice(int index)
    {
        choice = index;
    }

    public IEnumerator ExecuteScenes()
    {
        int index = 0;
        List<DialogueEvent> dialogueEvents = new List<DialogueEvent>();
        foreach (DialogueEvent e in events)
            if (e.requirementSet.RequirementsMet()
                && !Master.data.events.Contains(e.eventId))
                dialogueEvents.Add(e);
        if (dialogueEvents.Count > 0)
            StartCoroutine(PanelIn());

        foreach (DialogueEvent e in dialogueEvents)
        {
            dialogue = e.dialogues[0];
            ExecuteActions(dialogue.actorActions);
            yield return new WaitForSeconds(1);
            textBox.SetActive(true);
            header.text = dialogue.speaker;
            body.text = dialogue.message;
            image.gameObject.SetActive(dialogue.imageSprite.Count > 0);
            spriteIndex = 0;
            while (!Input.GetMouseButtonDown(0))
                yield return null;
            foreach (Fighter actor in actors)
                actor.SetAnimation(-1);

            yield return StartCoroutine(ExecuteScene(e, true));

            textBox.SetActive(false);
            actors[0].NewPos(Vector3.left * offscreenPos, false);
            actors[1].NewPos(Vector3.left * offscreenPos, false);
            actors[2].NewPos(Vector3.right * offscreenPos, false);
            actors[3].NewPos(Vector3.right * offscreenPos, false);
            if (index < dialogueEvents.Count - 1)
                index++;
            else
                StartCoroutine(PanelOut());
            yield return new WaitForSeconds(1);
        }
        isDone = true;
    }

    public IEnumerator ExecuteScene(DialogueEvent e, bool skipFirst = false)
    {
        for (int dialogueIndex = skipFirst ? 1 : 0;
                dialogueIndex < e.dialogues.Count;
                dialogueIndex++)
        {
            dialogue = e.dialogues[dialogueIndex];
            ExecuteActions(dialogue.actorActions);
            header.text = dialogue.speaker;
            body.text = dialogue.message;

            image.gameObject.SetActive(dialogue.imageSprite.Count > 0);
            // Check if the current spritesheet is different from the previous to reset the animation loop.
            if (dialogue.imageSprite.Count == 0
                || dialogueIndex == 0
                || e.dialogues[dialogueIndex - 1].imageSprite.Count == 0
                || e.dialogues[dialogueIndex - 1].imageSprite[0] != dialogue.imageSprite[0])
                spriteIndex = 0;

            yield return null;
            while (!Input.GetMouseButtonDown(0))
                yield return null;
            foreach (Fighter actor in actors)
                actor.SetAnimation();
        }

        if (e.choices.Count == 2)
        {
            choice = -1;
            textBox.SetActive(false);
            image.gameObject.SetActive(false);
            choiceBox.SetActive(true);
            choice1.text = e.choices[0].text;
            choice2.text = e.choices[1].text;
            while (choice < 0)
                yield return null;
            choiceBox.SetActive(false);
            textBox.SetActive(true);
            yield return ExecuteScene(e.choices[choice].leadsTo);
        }

        if (e.addId)
            Master.data.events.Add(e.eventId);
    }

    public IEnumerator PanelIn()
    {
        StopCoroutine("PanelOut");
        float panelFade = 0;
        while (panelFade < 1 && !Input.GetMouseButtonDown(0))
        {
            panelFade += Time.deltaTime;
            panel.color = Color.Lerp(Color.clear, panelColor, panelFade);
            yield return null;
        }
        panel.color = panelColor;
    }
    public IEnumerator PanelOut()
    {
        StopCoroutine("PanelIn");
        image.gameObject.SetActive(false);
        float panelFade = 1;
        while (panelFade > 0)
        {
            panelFade -= Time.deltaTime;
            panel.color = Color.Lerp(Color.clear, panelColor, panelFade);
            yield return null;
        }
        panel.color = Color.clear;
    }

    public void ExecuteActions
        (List<DialogueEvent.Dialogue.ActorAction> actions)
    {
        foreach (DialogueEvent.Dialogue.ActorAction action in actions)
        {
            actor = actors[action.actorIndex];
            if (action.changeUnit && actor.unit != action.changeUnit)
                actor.ChangeUnit(action.changeUnit, canBeBig: true);

            switch (action.behavior)
            {
                case DialogueEvent.Dialogue.ActorAction.ActorBehavior.Stage1:
                    actor.NewPos(Vector3.left * 7);
                    break;
                case DialogueEvent.Dialogue.ActorAction.ActorBehavior.Stage2:
                    actor.NewPos(Vector3.left * 4);
                    break;
                case DialogueEvent.Dialogue.ActorAction.ActorBehavior.Stage3:
                    actor.NewPos(Vector3.right * 4);
                    break;
                case DialogueEvent.Dialogue.ActorAction.ActorBehavior.Stage4:
                    actor.NewPos(Vector3.right * 7);
                    break;
                case DialogueEvent.Dialogue.ActorAction.ActorBehavior.OutLeft:
                    actor.NewPos(Vector3.left * offscreenPos);
                    break;
                case DialogueEvent.Dialogue.ActorAction.ActorBehavior.OutRight:
                    actor.NewPos(Vector3.right * offscreenPos);
                    break;
                case DialogueEvent.Dialogue.ActorAction.ActorBehavior.Attack:
                    actor.SetAttackAnimation(true);
                    break;
                case DialogueEvent.Dialogue.ActorAction.ActorBehavior.Ability:
                    actor.SetAbilityAnimation(true);
                    break;
                default:
                    Debug.LogWarning($"Unknown actor behavior: {action.behavior}");
                    break;
            }
        }
    }
}