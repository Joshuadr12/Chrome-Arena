using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData_0_3_2
{
    /// <summary>
    /// Stores player states for saving/loading.
    /// </summary>

    public string saveVersion = "0.3.2";
    public float battleSpeed, musicVolume, sfxVolume;
    public string character;
    public int week;
    public List<int> defaultSquads = new List<int>();
    public List<SquadData> squads = new List<SquadData>();
    public Dictionary<string, int> income = new Dictionary<string, int>(),
        resources = new Dictionary<string, int>();
    public Dictionary<string, int> upgrades = new Dictionary<string, int>();
    public Dictionary<string, List<string>> units = new Dictionary<string, List<string>>();
    public Dictionary<string, int> stars = new Dictionary<string, int>();
    public List<string> events = new List<string>();
    public int level, starsLeftover, upgradePoints, researchPoints;

    public PlayerData_0_3_2(Player player)
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

        SquadData squad;
        squads.Clear();
        foreach (Squad s in player.squads)
        {
            squad = new SquadData();
            squad.title = s.squadName;
            squad.colour = s.colour;
            squad.artifact = "Sword";
            //squad.artifact = s.artifact.name;

            List<string> newLine;
            squad.lines.Clear();
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

    [Serializable]
    public class SquadData
    {
        public string title, colour, artifact;
        public List<List<string>> lines = new List<List<string>>();
    }
}