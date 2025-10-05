using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Chrome Arena/Unit")]
public class Unit : ScriptableObject
{
    /// <summary>
    /// Represents a specific type of fighter to use in battle. (Pawn, Elite, etc.)
    /// </summary>

    // Variables.
    [Header("Stats")]
    public List<string> colours = new List<string>(){ "all" };
    public bool playable = true;
    public int health = 1;
    public int attack = 1;
    public int price = 1;
    public bool fast;
    public int agile = 0, block = 0, armor = 0;
    public bool antiAgile, antiBlock;
    public bool morph, combo;
    public bool slow, steady;
    public List<Ability> abilities;
    [Tooltip("Used in squad customization to describe what this unit does.")] public List<string> keywords;
    [Tooltip("A list of units to reference when describing what this unit does.")] public List<Unit> keywordUnits;
    [Header("Skeleton")]
    public bool useSkeleton = true;
    public Appearance appearance;
    public int attackId;
    public int abilityId;
    [Header("Spritesheet")]
    public Sprite[] idle;
    public Sprite[] move, abilityAnim, attackAnim, die;
    [Header("Miscellaneous")]
    public float bodySize = 1;

    public string GetDescription()
    {
        /// <summary>Generates and returns a multiline description about the unit for display.</summary>

        // Name, health/attack, and cost.
        string result = name;
        result += $", {health}/{attack}, {price}C";

        // Traits
        if (fast)
            result += $", Fast";
        if (agile > 0)
            result += $", Agile {agile}";
        if (block > 0)
            result += $", Block {block}";
        if (armor > 0)
            result += $", Armor {armor}";
        if (antiAgile)
            result += ", AntiAgile";
        if (antiBlock)
            result += ", AntiBlock";
        if (morph)
            result += ", Morph";
        if (combo)
            result += ", Combo";
        if (slow)
            result += ", Slow";
        if (steady)
            result += ", Steady";

        // Abilities
        foreach (Ability ability in abilities)
            result += $"\n{ability.description}";

        return result;
    }

    public string KeywordDescription()
    {
        /// <summary>Generates and returns a brief description about the unit for use in keyword descriptions.</summary>

        // Name, health, and attack.
        string result = name;
        result += $": {health}/{attack}";

        // Traits.
        if (fast)
            result += $", Fast";
        if (agile > 0)
            result += $", Agile {agile}";
        if (block > 0)
            result += $", Block {block}";
        if (armor > 0)
            result += $", Armor {armor}";
        if (antiAgile)
            result += ", AntiAgile";
        if (antiBlock)
            result += ", AntiBlock";
        if (morph)
            result += ", Morph";
        if (combo)
            result += ", Combo";
        if (slow)
            result += ", Slow";
        if (steady)
            result += ", Steady";

        return result;
    }
}

[Serializable]
public class Ability
{
    /// <summary>
    /// Represents a unit ability.
    /// </summary>

    // Serialized variables for the editor.
    [TextArea(2, 2)] public string description;
    public Cause cause;
    public List<Effect> effects;

    // Miscellaneous variables.
    [HideInInspector] public Fighter owner;
}

[Serializable]
public class Cause
{
    /// <summary>Represents the cause that activates a unit ability.</summary>

    // The specific event that activates an ability.
    public enum CauseType
    {
        Artifact,
        Summon,
        Step,
        Bonus,
        Block,
        Nonlethal,
        Death
    }

    // Variables.
    public CauseType type = CauseType.Summon;
    [Tooltip("The source of the cause")] public CauseUnit source;
    [Tooltip("The unit affected")] public CauseUnit target;
}

[Serializable]
public class Effect
{
    /// <summary>Represents the effect of a unit ability.</summary>

    // The specific action that an ability does.
    public enum EffectType
    {
        Damage,
        Buff,
        Summon,
        MoveFront,
        GainColor,
        GiveTrait,
        Retreat
    }

    // Variables.
    public EffectType type;
    public EffectUnit target;
    public int typeInt1, typeInt2;
    public string typeStr;
    public bool forOpponent;
    public Line typeUnits;

    public bool CanUse
        (Battle battle,
        Location reference,
        Fighter causeSource,
        Fighter causeTarget)
    {
        List<Fighter> targets = target.SelectTargets
            (battle,
            reference,
            causeSource,
            causeTarget);
        if (targets.Count == 0)
            return false;

        switch (type)
        {
            case EffectType.Damage:
                return typeInt1 > 0;
            case EffectType.Buff:
                return (typeInt1 != 0) || (typeInt2 != 0);
            case EffectType.Summon:
                return typeUnits.units.Count > 0;
            case EffectType.MoveFront:
                return reference.index > 0;
            case EffectType.GainColor:
                return typeInt1 != 0;
            case EffectType.GiveTrait:
                return (typeStr != "") && (typeInt1 != 0);
            case EffectType.Retreat:
                return true;
            default:
                Debug.LogError($"Unknown effect type {type}");
                return false;
        }
    }
}

[Serializable]
public class CauseUnit
{
    /// <summary>A set of filters for what fighters can activate a unit ability.</summary>

    // The specific fighters that can activate an ability.
    public enum CauseUnitType
    {
        Any,
        This,
        Enemy
    }
    // The specific lanes from which fighters can activate an ability.
    public enum CauseLane
    {
        This,
        Any
    }

    // Variables.
    public CauseUnitType unit;
    public CauseLane lane;

    public List<Fighter> UnitsPossible
        (Battle battle,
        Location reference)
    {
        /// <summary>Compiles and returns a list of all possible fighters that could activate an ability.</summary>
        /// <param name="battle">The battle in question.</param>
        /// <param name="reference">The location of the ability being activated.</param>

        List<Fighter> result = new List<Fighter>();
        result.Add(null);
        if (reference == null)
            return result;

        List<int> lanes = LanesPossible(battle, reference.lane);
        switch (unit)
        {
            case CauseUnitType.Any:
                foreach (int lane in lanes)
                {
                    foreach (GameObject fighter in battle.lanes[lane].left)
                        result.Add(fighter.GetComponent<Fighter>());
                    foreach (GameObject fighter in battle.lanes[lane].right)
                        result.Add(fighter.GetComponent<Fighter>());
                }
                return result;
            case CauseUnitType.This:
                result.Add(battle.GetFighter(reference));
                return result;
            case CauseUnitType.Enemy:
                foreach (int lane in lanes)
                {
                    foreach (GameObject fighter in battle.lanes[lane].left)
                        if (!reference.isLeft)
                            result.Add(fighter.GetComponent<Fighter>());
                    foreach (GameObject fighter in battle.lanes[lane].right)
                        if (reference.isLeft)
                            result.Add(fighter.GetComponent<Fighter>());
                }
                return result;
            default:
                Debug.LogError("Unknown case for CauseUnitType");
                break;
        }
        return result;
    }

    public List<int> LanesPossible(Battle battle, int reference)
    {
        /// <summary>Compiles and returns a list of indeces for all possible lanes from which fighters could activate an ability.</summary>
        /// <param name="battle">The battle in question.</param>
        /// <param name="reference">The index for the lane from which the ability is being activated.</param>

        List<int> result = new List<int>();
        switch (lane)
        {
            case CauseLane.This:
                result.Add(reference);
                return result;
            case CauseLane.Any:
                for (int l = 0; l < battle.lanes.Count; l++)
                    result.Add(l);
                return result;
            default:
                Debug.LogError("Unknown case for CauseLane");
                break;
        }
        return result;
    }
}

[Serializable]
public class EffectUnit
{
    /// <summary>A set of filters for what fighters can be targeted by a unit ability.</summary>

    // The specific fighters that can be targeted by an ability.
    public enum EffectUnitType
    {
        All,
        Random,
        Source,
        Target,
        NearestAhead,
        Front
    }
    // The specific lanes from which fighters can be targeted by an ability.
    public enum EffectLane
    {
        This,
        All,
        Any
    }

    public EffectUnitType unit;
    public EffectLane lane;
    public bool targetSelf = true, targetAllies = true,
        targetEnemies = true, targetDead;
    public int unitCount = 1;

    public List<Fighter> SelectTargets
        (Battle battle,
        Location reference,
        Fighter causeSource,
        Fighter causeTarget)
    {
        /// <summary>Compiles and returns a list of all possible fighters that could be targeted by an ability.</summary>
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
        if (unit == EffectUnitType.Source)
        {
            if (targetDead || (causeSource.health > 0))
                result.Add(causeSource);
        }
        else if (unit == EffectUnitType.Target)
        {
            if (targetDead || (causeTarget.health > 0))
                result.Add(causeTarget);
        }
        else
        {
            List<Fighter> GetFightersFromLane(int lane)
            {
                List<Fighter> result = new List<Fighter>();
                if
                    ((reference.isLeft && targetAllies)
                    || (!reference.isLeft && targetEnemies))
                    foreach (GameObject f in battle.lanes[lane].left)
                        result.Add(f.GetComponent<Fighter>());

                if
                    ((reference.isLeft && targetEnemies)
                    || (!reference.isLeft && targetAllies))
                    foreach (GameObject f in battle.lanes[lane].right)
                        result.Add(f.GetComponent<Fighter>());

                return result;
            }

            List<int> lanes = SelectLanes(battle, reference.lane);
            if (lane == EffectLane.Any)
            {
                foreach (int lane in lanes)
                    foreach (Fighter f in GetFightersFromLane(lane))
                        result.Add(f);
                result = SelectFightersFromList(battle, reference, result);
            }
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
        }

        while (result.Contains(referenceF))
            result.Remove(referenceF);
        if
            (targetSelf && (targetDead
            || (referenceF.health > 0)))
            result.Add(referenceF);

        return result;
    }

    public List<Fighter> SelectFightersFromList
        (Battle battle,
        Location reference,
        List<Fighter> list)
    {
        /// <summary>Compiles and returns a list of all possible fighters from the given list that could be targeted by an ability.</summary>
        /// <param name="battle">The battle in quesiton.</param>
        /// <param name="reference">The location of the ability.</param>
        /// <param name="list">The list of fighters to reference.</param>

        // If the effect does not target dead fighters, remove them from the list.
        if (!targetDead)
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

        switch (unit)
        {
            // All
            case EffectUnitType.All:
                return list;

            // Random
            case EffectUnitType.Random:
                Fighter temp;
                for
                    (int n = Mathf.Min(list.Count, unitCount);
                    n > 0;
                    n--)
                {
                    temp = list[UnityEngine.Random.Range(0, list.Count)];
                    result.Add(temp);
                    list.Remove(temp);
                }
                return result;

            // Nearest ahead
            case EffectUnitType.NearestAhead:
                int targetIndex = reference.index - 1;
                int targetsLeft = unitCount;

                while ((targetIndex >= 0) && (targetsLeft > 0))
                {
                    foreach (Fighter f in GetFightersInColumn
                        (reference.isLeft,
                        targetIndex,
                        targetsLeft))
                    {
                        result.Add(f);
                        targetsLeft--;
                    }
                    targetIndex--;
                }

                targetIndex = 0;
                while ((targetIndex < 10) && (targetsLeft > 0))
                {
                    foreach (Fighter f in GetFightersInColumn
                        (!reference.isLeft,
                        targetIndex,
                        targetsLeft))
                    {
                        result.Add(f);
                        targetsLeft--;
                    }
                    targetIndex++;
                }
                return result;

            // Front
            case EffectUnitType.Front:
                for (int i = 0; i < 10; i++)
                    foreach (Fighter f in GetFightersInColumn
                        (reference.isLeft,
                        i,
                        unitCount - result.Count))
                        result.Add(f);
                return result;

            default:
                Debug.LogError("Unknown or improper case for EffectUnitType");
                break;
        }
        return result;
    }

    public List<int> SelectLanes(Battle battle, int reference)
    {
        /// <summary>Compiles and returns a list of indices for all possible lanes where fighters could be targeted by an ability.</summary>
        /// <param name="battle">The battle in quesiton.</param>
        /// <param name="reference">The index for the lane from which the ability is being activated.</param>

        List<int> result = new List<int>();
        switch (lane)
        {
            case EffectLane.This:
                result.Add(reference);
                return result;
            case EffectLane.All:
                for (int l = 0; l < battle.lanes.Count; l++)
                    result.Add(l);
                return result;
            case EffectLane.Any:
                for (int l = 0; l < battle.lanes.Count; l++)
                    result.Add(l);
                return result;
            default:
                Debug.LogWarning("Unknown or improper case for EffectLane");
                break;
        }
        return result;
    }
}

[Serializable]
public class Appearance
{
    public GameObject model;
    public List<SpriteSet> itemSet, eyeSet, hairSet, bodySet,
        clothSet, armorSet, pantSet, weaponSet, backSet, horseSet;

    [Serializable]
    public class SpriteSet
    {
        /// <summary>
        /// Stores information for a single sprite renderer for a unit display.
        /// </summary>

        public Sprite sprite;
        public Color color = Color.white;

        public void UpdateRenderer(SpriteRenderer renderer)
        {
            renderer.sprite = sprite;
            renderer.color = color;
        }
    }
}

public class Trigger
{
    /// <summary>
    /// Represents a single instance of a unit ability being activated.
    /// </summary>

    // Variables.
    public Ability ability;
    public Fighter causeSource, causeTarget;

    public static int CompareTo(Trigger a, Trigger b)
    {
        Ability x = a.ability, y = b.ability;
        int z = Master.abilityCauses.IndexOf(x.cause.type),
            w = Master.abilityCauses.IndexOf(y.cause.type);
        int result = z.CompareTo(w);
        if (result == 0)
        {
            for
                (int i = 0;
                i < Math.Min(x.effects.Count, y.effects.Count);
                i++)
            {
                z = Master
                    .abilityEffects
                    .IndexOf(x.effects[i].type);
                w = Master
                    .abilityEffects
                    .IndexOf(y.effects[i].type);
                result = z.CompareTo(w);
                if (result != 0)
                    return result;
            }
            result = x
                .effects
                .Count
                .CompareTo(y.effects.Count);
            if (result == 0)
                result = x
                    .owner
                    .unit
                    .name
                    .CompareTo(y.owner.unit.name);
        }
        return result;
    }

    public bool CanUse(Battle battle)
    {
        if
            (ability.owner.steady
            && UnityEngine.Random.value <= 0.5f)
            return false;

        Location reference = battle.GetLocation(ability.owner);
        foreach (Effect effect in ability.effects)
            if (!effect.CanUse
                (battle,
                reference,
                causeSource,
                causeTarget))
                return false;

        return true;
    }
}