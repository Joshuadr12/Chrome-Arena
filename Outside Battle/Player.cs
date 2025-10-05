using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Chrome Arena/Player Data")]
public class Player : ScriptableObject
{
    /// <summary>
    /// Stores current player state.
    /// </summary>

    public float battleSpeed = 1, musicVolume = 1, sfxVolume = 1;
    public int day;
    [Tooltip("The unit representing the player that displays in battle.")] public Unit character;
    public List<int> defaultSquads = new List<int>();
    [Tooltip("The resources that the player has at the start of each day.")] public List<ResourceQuantity> income;
    public List<ResourceQuantity> resources;
    [FormerlySerializedAs("totalSquads")] public List<Squad> squads;
    public List<LevelStatus> stars;
    public List<string> events;
    public int[] level;

    public void AddResource(string colour, int amount)
    {
        for (int i = 0; i < resources.Count; i++)
            if (resources[i].colour == colour)
                resources[i].quantity += amount;
    }

    public void NextDay()
    {
        Master.data.day++;
        resources.Clear();
        foreach (ResourceQuantity gain in income)
            resources.Add(new ResourceQuantity(gain.colour, gain.quantity));
    }

    [Serializable]
    public class ResourceQuantity
    {
        public string colour;
        public int quantity;

        public ResourceQuantity(string colour, int quantity)
        {
            this.colour = colour;
            this.quantity = quantity;
        }
    }

    [Serializable]
    public class LevelStatus
    {
        public string levelId;
        public int stars;

        public LevelStatus(string levelId, int stars)
        {
            this.levelId = levelId;
            this.stars = stars;
        }
    }

    [Serializable]
    public class SquadData
    {
        // TODO: Contruction methods

        public string title, colour, artifact;
        public List<LineData> lines = new List<LineData>();

        [Serializable]
        public class LineData
        {
            public List<string> units = new List<string>();
        }
    }
}