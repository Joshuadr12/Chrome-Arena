using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Chrome Arena/Unit")]
public class Unit : ScriptableObject
{
    /// <summary>
    /// Represents a specific type of fighter to use in battle. (Pawn, Elite, etc.)
    /// </summary>

    // Variables.
    [Header("Stats")]
    public List<string> colours = new List<string>(){ "all" };
    [Tooltip("The units that can be unlocked when research is conducted with this unit.")] public List<Unit> research;
    public bool isStarter;
    [Tooltip("A brief description of the unit's abilities when referenced by other units.")] public string abilityDesc;
    public float bodySize = 1;
    [Header("Battle")]
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
    public GameObject model;
    public List<SpriteSet> bodySet;
    public string attackString = "Attack_0";
    public string abilityString = "Skill_0";
    [Header("Spritesheet")]
    public Sprite[] idle;
    public Sprite[] move, abilityAnim, attackAnim, die;

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
        if (abilityDesc != "")
            result += ", " + abilityDesc;

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
        Death,
        Pillage
    }

    // Variables.
    public CauseType type = CauseType.Summon;
    [Tooltip("The source of the cause"), FormerlySerializedAs("newSource")] public AbilityTarget source;
    [Tooltip("The unit affected"), FormerlySerializedAs("newTarget")] public AbilityTarget target;
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
        GainPaint,
        GiveTrait,
        Retreat,
        Bleach
    }

    // Variables.
    public EffectType type;
    [FormerlySerializedAs("newTarget")] public AbilityTarget target;
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
            case EffectType.GainPaint:
                return typeInt1 != 0;
            case EffectType.GiveTrait:
                return (typeStr != "") && (typeInt1 != 0);
            case EffectType.Retreat:
                return true;
            case EffectType.Bleach:
                return true;
            default:
                Debug.LogError($"Unknown effect type {type}");
                return false;
        }
    }
}

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