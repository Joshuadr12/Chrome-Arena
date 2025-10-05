using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

[CreateAssetMenu(menuName = "Chrome Arena/Weapon")]
public class Artifact : ScriptableObject
{
    /// <summary>
    /// Represents a specific type of artifact for the player to use in battle. (Bow, Sword, etc.)
    /// </summary>

    public Sprite sprite;
    public List<string> colours = new List<string>(){ "all" };
    public bool playable = true;
    public Ability ability;
    [Tooltip("Used in squad customization to describe what this unit does.")] public List<string> keywords;
    [Tooltip("A list of units to reference when describing what this unit does.")] public List<Unit> keywordUnits;

    public string GetDescription()
    {
        /// <summary>Generates and returns a multiline description about the artifact for display.</summary>

        return ability.description;
    }
}