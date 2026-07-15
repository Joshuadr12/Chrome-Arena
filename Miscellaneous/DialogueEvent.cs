using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(menuName = "Chrome Arena/Dialogue Scene")]
public class DialogueEvent : ScriptableObject
{
    public string eventId;
    public bool addId = true;
    public RequirementSet requirementSet;
    public List<Dialogue> dialogues;
    [Tooltip("There should be zero or two elements in this list.")] public List<Choice> choices;

    [Serializable]
    public class Dialogue
    {
        public string speaker;
        [TextArea(2, 4)] public string message;
        public List<Sprite> imageSprite;
        public List<ActorAction> actorActions;

        [Serializable]
        public class ActorAction
        {
            public enum ActorBehavior
            {
                Stage1,
                Stage2,
                Stage3,
                Stage4,
                OutLeft,
                OutRight,
                Attack,
                Ability
            }

            public int actorIndex;
            public ActorBehavior behavior;
            public Unit changeUnit;
        }
    }

    [Serializable]
    public class Choice
    {
        public string text;
        public DialogueEvent leadsTo;
    }
}