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
    [FormerlySerializedAs("day")] public int week;
    [Tooltip("The unit representing the player that displays in battle.")] public Unit character;
    public List<int> defaultSquads = new List<int>();
    [FormerlySerializedAs("totalSquads")] public List<Squad> squads;
    public List<ArtifactList> artifacts, forgeSales;
    [Tooltip("The resources that the player has at the start of each day.")] public List<ResourceQuantity> income;
    public List<ResourceQuantity> resources;
    public List<UpgradeQuantity> upgrades;
    public List<UnitColours> units;
    public List<BattleStars> stars;
    public List<string> events;
    public int level, starsLeftover,
        upgradePoints, researchPoints, artifactsPurchased;

    public void AddResource(string colour, int amount)
    {
        for (int i = 0; i < resources.Count; i++)
            if (resources[i].colour == colour)
                resources[i].quantity += amount;
    }

    public void NextWeek()
    {
        week++;
        artifactsPurchased = 0;

        // Reset paint resources.
        resources.Clear();
        foreach (ResourceQuantity gain in income)
            resources.Add(new ResourceQuantity(gain.colour, gain.quantity));

        // Restock the Forge.
        if (events.Contains("first_purchase"))
        {
            foreach (ArtifactList list in forgeSales)
                list.artifacts.Clear();
            ArtifactType artifactType;
            Artifact artifact;
            for (int a = 0; a < 3; a++)
            {
                artifactType = Master.artifacts
                    [UnityEngine.Random.Range(0, Master.artifacts.Count)];
                artifact = new Artifact
                    (artifactType,
                    Mathf.RoundToInt(50 / artifactType.valuePerUse));
                forgeSales[UnityEngine.Random.Range(0, forgeSales.Count)]
                    .artifacts.Add(artifact);
            }
        }
    }

    public int TimesUpgraded(Upgrade upgrade)
    {
        foreach (UpgradeQuantity u in upgrades)
            if (u.upgradeId == upgrade.upgradeId)
                return u.quantity;
        return 0;
    }
    public int TimesUpgraded(string upgradeId)
    {
        foreach (UpgradeQuantity u in upgrades)
            if (u.upgradeId == upgradeId)
                return u.quantity;
        return 0;
    }

    public bool HasUnitColour (Unit unit, string colour)
    {
        foreach (UnitColours u in units)
            if (u.unitName == unit.name)
                return colour == "all"
                    || Master.TranslateCollections(u.colours)
                    .Contains(colour);
        return false;
    }

    public void MakeUpgrade(Upgrade upgrade)
    {
        foreach (UpgradeQuantity u in upgrades)
        {
            if (u.upgradeId == upgrade.upgradeId)
            {
                u.quantity++;
                return;
            }
        }
        upgrades.Add(new UpgradeQuantity(upgrade.upgradeId, 1));
    }

    public int ArtifactsOwned(string colour)
    {
        foreach (ArtifactList list in artifacts)
            if (list.colour == colour)
                return list.artifacts.Count;
        return 0;
    }

    public bool NewUnitColour(UnitColour unit)
    {
        foreach (UnitColours u in units)
        {
            if (u.unitName == unit.unit.name)
            {
                return u.NewColour(unit.colour);
            }
        }

        UnitColours newUnit = new UnitColours(unit.unit.name, new List<string>());
        newUnit.colours.Add("neutral");
        newUnit.colours.Add(unit.colour);
        units.Add(newUnit);
        return true;
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
    public class UpgradeQuantity
    {
        public string upgradeId;
        public int quantity;

        public UpgradeQuantity(string upgradeId, int quantity)
        {
            this.upgradeId = upgradeId;
            this.quantity = quantity;
        }
    }

    [Serializable]
    public class UnitColours
    {
        public string unitName;
        public List<string> colours;

        public UnitColours(string unit, List<string> colours)
        {
            unitName = unit;
            this.colours = colours;
        }

        public bool NewColour(string colour)
        {
            if (!colours.Contains(colour))
            {
                colours.Add(colour);
                return true;
            }
            return false;
        }
    }

    [Serializable]
    public class BattleStars
    {
        [FormerlySerializedAs("levelId")] public string battleId;
        public int stars;

        public BattleStars(string battleId, int stars)
        {
            this.battleId = battleId;
            this.stars = stars;
        }
    }
}

[Serializable]
public class ArtifactList
{
    public string colour;
    public List<Artifact> artifacts;

    public ArtifactList(string colour, List<Artifact> artifacts)
    {
        this.colour = colour;
        this.artifacts = artifacts;
    }
}