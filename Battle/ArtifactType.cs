using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Chrome Arena/Artifact")]
public class ArtifactType : ScriptableObject
{
    /// <summary>
    /// Represents a specific type of artifact for the player to use in battle. (Bow, Sword, etc.)
    /// </summary>

    public Sprite sprite;
    public List<string> colours = new List<string>(){ "all" };
    public bool playable = true;
    public int valuePerUse = 10;
    public Ability ability;
    [Tooltip("Used in squad customization to describe what this unit does.")] public List<string> keywords;
    [Tooltip("A list of units to reference when describing what this unit does.")] public List<Unit> keywordUnits;

    public string GetDescription(bool addName = true)
    {
        /// <summary>Generates and returns a multiline description about the artifact for display.</summary>

        string result = addName ? $"{name.ToUpper()}: " : "";
        result += ability.description;
        return result;
    }
}

[Serializable]
public class Artifact
{
    public ArtifactType type;
    public int uses = 5;

    public Artifact(ArtifactType type, int uses)
    {
        this.type = type;
        this.uses = uses;
    }

    public string GetDescription()
    {
        string result = $"{type.name}, costs {type.valuePerUse}A, {uses} use";
        if (uses != 1)
            result += "s";
        result += $"\n{type.GetDescription(false)}";
        return result;
    }
}