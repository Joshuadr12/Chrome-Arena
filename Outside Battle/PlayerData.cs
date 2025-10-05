using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData
{
    /// <summary>
    /// Stores player states for saving/loading.
    /// </summary>

    public float battleSpeed, musicVolume, sfxVolume;
    public string character;
    public int day;
    public List<int> defaultSquads = new List<int>();
    public List<Player.SquadData> squads = new List<Player.SquadData>();
    public List<Player.ResourceQuantity> income = new List<Player.ResourceQuantity>();
    public List<Player.ResourceQuantity> resources = new List<Player.ResourceQuantity>();
    public List<Player.LevelStatus> stars = new List<Player.LevelStatus>();
    public List<string> events = new List<string>();
    public int[] level;

    public PlayerData(Player player)
    {
        battleSpeed = player.battleSpeed;
        musicVolume = player.musicVolume;
        sfxVolume = player.sfxVolume;
        character = player.character.name;
        day = player.day;

        defaultSquads.Clear();
        foreach (int i in player.defaultSquads)
            defaultSquads.Add(i);

        income.Clear();
        foreach (Player.ResourceQuantity colour in player.income)
            income.Add(colour);

        resources.Clear();
        foreach (Player.ResourceQuantity colour in player.resources)
            resources.Add(colour);

        stars.Clear();
        foreach (Player.LevelStatus star in player.stars)
            stars.Add(star);

        events.Clear();
        foreach (string e in player.events)
            events.Add(e);

        level = new int[2];
        level[0] = player.level[0];
        level[1] = player.level[1];

        Player.SquadData squad;
        squads.Clear();
        foreach (Squad s in player.squads)
        {
            squad = new Player.SquadData();
            squad.title = s.squadName;
            squad.colour = s.colour;
            squad.artifact = s.artifact.name;

            Player.SquadData.LineData newLine;
            squad.lines.Clear();
            foreach (Line line in s.units)
            {
                newLine = new Player.SquadData.LineData();
                foreach (Unit unit in line.units)
                    newLine.units.Add(unit.name);
                squad.lines.Add(newLine);
            }

            squads.Add(squad);
        }
    }
}