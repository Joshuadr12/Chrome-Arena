using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Chrome Arena/Ability Target")]
public class AbilityTarget : ScriptableObject
{
    /// <summary>A set of filters to select fighters for an ability.</summary>

    // The specific fighters that can be targeted by an ability.
    public enum FighterSelector
    {
        All,
        Random,
        Source,
        Target,
        Front,
        Last
    }
    // The specific lanes from which fighters can be targeted by an ability.
    public enum LaneSelector
    {
        All,
        Any
    }

    [Header("Lane Selection")]
    public bool useThisLane = true;
    public bool useOtherLanes;
    [Tooltip("Choose how to select which lanes.")] public LaneSelector laneSelector;
    public int laneCount = 1;

    [Header("Fighter Selection")]
    [Tooltip("Includes this fighter and allies in the same column.")] public bool alliesHere;
    public bool alliesBehind, alliesAhead, enemies;
    [Tooltip("Choose how to select which fighters.")] public FighterSelector fighterSelector;
    public int fighterCount = 1;

    [Header("Miscellaneous")]
    [Tooltip("Includes non-fighter entities such as artifacts; always leave false for effect targets.")] public bool allowNull;
    public bool allowDead;

    public List<Fighter> SelectTargets
        (Battle battle,
        Location reference,
        Fighter causeSource = null,
        Fighter causeTarget = null)
    {
        /// <summary>Compiles and returns a list of all possible fighters to be selected for the ability.</summary>
        /// <param name="battle">The battle in question.</param>
        /// <param name="reference">The location of the ability.</param>
        /// <param name="causeSource">The source of the cause that activated the ability.</param>
        /// <param name="causeTarget">The target of the cause that activated the ability.</param>

        Fighter referenceF;
        if (causeTarget && causeTarget.isArtifact)
        {
            referenceF = causeTarget;
            reference = new Location();
            reference.lane = 0;
            reference.isLeft = referenceF.isLeft;
            reference.index = 0;
        }
        else
            referenceF = battle.GetFighter(reference);

        List<Fighter> result = new List<Fighter>();
        List<Fighter> possible = new List<Fighter>();
        List<int> lanes = SelectLanes(battle, reference.lane);

        List<Fighter> GetFightersFromLane(int lane)
        {
            List<Fighter> result = new List<Fighter>();

            // When the reference fighter is left.
            if (referenceF.isLeft)
            {
                if (alliesAhead)
                {
                    for (int i = 0; i < reference.index; i++)
                        result.Add(battle
                            .lanes[reference.lane]
                            .left[i]
                            .GetComponent<Fighter>());
                }
                if (alliesHere)
                    result.Add(battle
                        .lanes[reference.lane]
                        .left[reference.index]
                        .GetComponent<Fighter>());
                if (alliesBehind)
                {
                    for (int i = reference.index + 1;
                        i < battle.lanes[reference.lane].left.Count;
                        i++)
                        result.Add(battle
                            .lanes[reference.lane]
                            .left[i]
                            .GetComponent<Fighter>());
                }
                if (enemies)
                {
                    for (int i = 0;
                        i < battle.lanes[reference.lane].right.Count;
                        i++)
                        result.Add(battle
                            .lanes[reference.lane]
                            .right[i]
                            .GetComponent<Fighter>());
                }
            }

            // When the reference fighter is right.
            else
            {
                if (alliesAhead)
                {
                    for (int i = 0; i < reference.index; i++)
                        result.Add(battle
                            .lanes[reference.lane]
                            .right[i]
                            .GetComponent<Fighter>());
                }
                if (alliesHere)
                    result.Add(battle
                        .lanes[reference.lane]
                        .right[reference.index]
                        .GetComponent<Fighter>());
                if (alliesBehind)
                {
                    for (int i = reference.index + 1;
                        i < battle.lanes[reference.lane].right.Count;
                        i++)
                        result.Add(battle
                            .lanes[reference.lane]
                            .right[i]
                            .GetComponent<Fighter>());
                }
                if (enemies)
                {
                    for (int i = 0;
                        i < battle.lanes[reference.lane].left.Count;
                        i++)
                        result.Add(battle
                            .lanes[reference.lane]
                            .left[i]
                            .GetComponent<Fighter>());
                }
            }

            return result;
        }

        // When the lane selector is "Any," fighters from all lanes are combined for one selection.
        Debug.Log($"Owner = {battle.GetFighter(reference).unit.name}; Lane selector = {laneSelector}.");
        if (laneSelector == LaneSelector.Any)
        {
            foreach (int lane in lanes)
                foreach (Fighter f in GetFightersFromLane(lane))
                    result.Add(f);
            result = SelectFightersFromList(battle, reference, result);
        }

        // Otherwise, an individual selection is made for each selected lane.
        else
        {
            foreach (int lane in lanes)
            {
                foreach (Fighter f in SelectFightersFromList
                    (battle,
                    reference,
                    GetFightersFromLane(lane)))
                    result.Add(f);
            }
        }

        // If the selection allows non-fighter entities, add it.
        if (allowNull)
            result.Add(null);
        return result;
    }

    public List<Fighter> SelectFightersFromList
        (Battle battle,
        Location reference,
        List<Fighter> list,
        Fighter causeSource = null,
        Fighter causeTarget = null)
    {
        /// <summary>Compiles and returns a list of all possible fighters from the given list that to be selected for the ability.</summary>
        /// <param name="battle">The battle in quesiton.</param>
        /// <param name="reference">The location of the ability.</param>
        /// <param name="list">The list of fighters to reference.</param>

        Debug.Log(list.Count);
        // If the selection does not allow dead fighters, remove them from the list.
        if (!allowDead)
        {
            List<Fighter> corpses = new List<Fighter>();
            foreach (Fighter f in list)
                if (f.health <= 0)
                    corpses.Add(f);
            foreach (Fighter f in corpses)
                list.Remove(f);
        }

        List<Fighter> result = new List<Fighter>();
        Fighter fighter;

        List<Fighter> GetFightersInColumn
            (bool isLeft,
            int index,
            int max)
        {
            List<Fighter> result = new List<Fighter>();
            foreach (Battle.Lane l in battle.lanes)
            {
                if (isLeft && (l.left.Count > index))
                {
                    fighter = l
                        .left[index]
                        .GetComponent<Fighter>();
                    if (list.Contains(fighter))
                        result.Add(fighter);
                }
                else if (!isLeft && (l.right.Count > index))
                {
                    fighter = l
                        .right[index]
                        .GetComponent<Fighter>();
                    if (list.Contains(fighter))
                        result.Add(fighter);
                }
            }

            while (result.Count > max)
                result.RemoveAt(UnityEngine.Random.Range(0, result.Count));
            return result;
        }

        switch (fighterSelector)
        {
            // All
            case FighterSelector.All:
                return list;

            // Random
            case FighterSelector.Random:
                if (list.Count <= fighterCount)
                    return list;

                Fighter temp;
                for
                    (int n = Mathf.Min(list.Count, fighterCount);
                    n > 0;
                    n--)
                {
                    temp = list[UnityEngine.Random.Range(0, list.Count)];
                    result.Add(temp);
                    list.Remove(temp);
                }
                return result;

            // Source
            case FighterSelector.Source:
                if (list.Contains(causeSource))
                    result.Add(causeSource);
                return result;

            // Target
            case FighterSelector.Target:
                if (list.Contains(causeTarget))
                    result.Add(causeTarget);
                return result;

            // First
            case FighterSelector.Front:
                if (list.Count <= fighterCount)
                    return list;

                for (int i = 0; i < 10; i++)
                {
                    foreach (Fighter f in GetFightersInColumn
                        (true, i, fighterCount - result.Count))
                        if (list.Contains(f)
                            && result.Count < fighterCount)
                            result.Add(f);
                    foreach (Fighter f in GetFightersInColumn
                        (false, i, fighterCount - result.Count))
                        if (list.Contains(f)
                            && result.Count < fighterCount)
                            result.Add(f);
                }
                return result;

            // Last
            case FighterSelector.Last:
                if (list.Count <= fighterCount)
                    return list;

                for (int i = 9; i >= 0; i--)
                {
                    foreach (Fighter f in GetFightersInColumn
                        (true, i, fighterCount - result.Count))
                        if (list.Contains(f)
                            && result.Count < fighterCount)
                            result.Add(f);
                    foreach (Fighter f in GetFightersInColumn
                        (false, i, fighterCount - result.Count))
                        if (list.Contains(f)
                            && result.Count < fighterCount)
                            result.Add(f);
                }
                return result;

            default:
                Debug.LogError($"Unknown or improper case for FighterSelector {fighterSelector}; returning all fighters.");
                break;
        }
        return result;
    }

    public List<int> SelectLanes(Battle battle, int reference)
    {
        /// <summary>Compiles and returns a list of indices for all possible lanes where fighters could be targeted by an ability.</summary>
        /// <param name="battle">The battle in quesiton.</param>
        /// <param name="reference">The index for the lane from which the ability is being activated.</param>

        // Add all possible lanes.
        List<int> result = new List<int>();
        if (useThisLane)
            result.Add(reference);
        for (int l = 0; l < battle.lanes.Count; l++)
            if (useOtherLanes && l != reference)
            {
                Debug.Log(l);
                result.Add(l);
            }

        // Select which lanes.
        switch (laneSelector)
        {
            case LaneSelector.All:
                return result;
            case LaneSelector.Any:
                return result;
            default:
                Debug.LogWarning($"Unknown or improper case for lane selector {laneSelector}; returning all lanes.");
                break;
        }
        return result;
    }
}