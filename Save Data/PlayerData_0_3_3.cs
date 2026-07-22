using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData_0_3_3
{
    /// <summary>
    /// Stores player states for saving/loading.
    /// </summary>

    public string saveVersion = "0.3.3";
    public float battleSpeed, musicVolume, sfxVolume;
    public string character;
    public int week;
    public List<int> defaultSquads = new List<int>();
    public List<SquadData> squads = new List<SquadData>();
    public Dictionary<string, List<ArtifactData>> artifacts = new Dictionary<string, List<ArtifactData>>(),
        forgeSales = new Dictionary<string, List<ArtifactData>>();
    public Dictionary<string, int> income = new Dictionary<string, int>(),
        resources = new Dictionary<string, int>();
    public Dictionary<string, int> upgrades = new Dictionary<string, int>();
    public Dictionary<string, List<string>> units = new Dictionary<string, List<string>>();
    public Dictionary<string, int> stars = new Dictionary<string, int>();
    public List<string> events = new List<string>();
    public int level, starsLeftover,
        upgradePoints, researchPoints, artifactsPurchased;

    public PlayerData_0_3_3() { }
    public PlayerData_0_3_3(Player player)
    {
        if (player == null)
            return;

        battleSpeed = player.battleSpeed;
        musicVolume = player.musicVolume;
        sfxVolume = player.sfxVolume;
        character = player.character.name;
        week = player.week;

        defaultSquads.Clear();
        foreach (int i in player.defaultSquads)
            defaultSquads.Add(i);

        List<ArtifactData> artifactData;
        artifacts.Clear();
        foreach (ArtifactList list in player.artifacts)
        {
            artifactData = new List<ArtifactData>();
            foreach (Artifact artifact in list.artifacts)
                artifactData.Add(new ArtifactData(artifact.type.name, artifact.uses));
            artifacts.Add(list.colour, artifactData);
        }

        forgeSales.Clear();
        foreach (ArtifactList list in player.forgeSales)
        {
            artifactData = new List<ArtifactData>();
            foreach (Artifact artifact in list.artifacts)
                artifactData.Add(new ArtifactData(artifact.type.name, artifact.uses));
            forgeSales.Add(list.colour, artifactData);
        }

        income.Clear();
        foreach (Player.ResourceQuantity colour in player.income)
            income.Add(colour.colour, colour.quantity);

        resources.Clear();
        foreach (Player.ResourceQuantity colour in player.resources)
            resources.Add(colour.colour, colour.quantity);

        upgrades.Clear();
        foreach (Player.UpgradeQuantity upgrade in player.upgrades)
            upgrades.Add(upgrade.upgradeId, upgrade.quantity);

        units.Clear();
        foreach (Player.UnitColours unit in player.units)
            units.Add(unit.unitName, unit.colours);

        stars.Clear();
        foreach (Player.BattleStars star in player.stars)
            stars.Add(star.battleId, star.stars);

        events.Clear();
        foreach (string e in player.events)
            events.Add(e);

        level = player.level;
        starsLeftover = player.starsLeftover;
        upgradePoints = player.upgradePoints;
        researchPoints = player.researchPoints;
        artifactsPurchased = player.artifactsPurchased;

        SquadData squad;
        squads.Clear();
        foreach (Squad s in player.squads)
        {
            squad = new SquadData(s.squadName, s.colour, new List<List<string>>());

            List<string> newLine;
            foreach (Line line in s.units)
            {
                newLine = new List<string>();
                foreach (Unit unit in line.units)
                    newLine.Add(unit.name);
                squad.lines.Add(newLine);
            }

            squads.Add(squad);
        }
    }

    public PlayerData_0_3_3(PlayerData_0_3_2 oldData)
    {
        if (oldData == null)
            return;

        battleSpeed = oldData.battleSpeed;
        musicVolume = oldData.musicVolume;
        sfxVolume = oldData.sfxVolume;
        character = oldData.character;
        week = oldData.week;

        defaultSquads.Clear();
        foreach (int i in oldData.defaultSquads)
            defaultSquads.Add(i);

        squads.Clear();
        foreach (PlayerData_0_3_2.SquadData s in oldData.squads)
            squads.Add(new SquadData(s.title, s.colour, s.lines));

        List<ArtifactData> list;
        artifacts.Clear();
        foreach (ArtifactList artifactList in Master.newData.artifacts)
        {
            list = new List<ArtifactData>();
            foreach (Artifact a in artifactList.artifacts)
                list.Add(new ArtifactData(a.type.name, a.uses));
            artifacts.Add(artifactList.colour, list);
        }

        forgeSales.Clear();
        foreach (ArtifactList sales in Master.newData.forgeSales)
        {
            list = new List<ArtifactData>();
            foreach (Artifact a in sales.artifacts)
                list.Add(new ArtifactData(a.type.name, a.uses));
            forgeSales.Add(sales.colour, list);
        }

        income.Clear();
        foreach (string colour in oldData.income.Keys)
            income.Add(colour, oldData.income[colour]);

        resources.Clear();
        foreach (string colour in oldData.resources.Keys)
            resources.Add(colour, oldData.resources[colour]);

        upgrades.Clear();
        foreach (string upgrade in oldData.upgrades.Keys)
            upgrades.Add(upgrade, oldData.upgrades[upgrade]);

        units.Clear();
        foreach (string unit in oldData.units.Keys)
            units.Add(unit, oldData.units[unit]);

        stars.Clear();
        foreach (string star in oldData.stars.Keys)
            stars.Add(star, oldData.stars[star]);

        events.Clear();
        foreach (string e in oldData.events)
            events.Add(e);

        level = oldData.level;
        starsLeftover = oldData.starsLeftover;
        upgradePoints = oldData.upgradePoints;
        researchPoints = oldData.researchPoints;
        artifactsPurchased = 0;
    }

    [Serializable]
    public class SquadData
    {
        public string title, colour;
        public List<List<string>> lines = new List<List<string>>();

        public SquadData(string title, string colour, List<List<string>> lines)
        {
            this.title = title;
            this.colour = colour;
            this.lines = lines;
        }
    }

    [Serializable]
    public class ArtifactData
    {
        public string name;
        public int uses;

        public ArtifactData(string name, int uses)
        {
            this.name = name;
            this.uses = uses;
        }
    }
}