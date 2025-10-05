using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Fighter : UnitDisplay
{
    /// <summary>
    /// Represents a single fighter in battle.
    /// </summary>

    // Serialized variables for the editor.
    [SerializeField] bool forCombat = true;
    [SerializeField] Transform statCanvas;
    [SerializeField] Image healthImage;
    [SerializeField] TMP_Text healthText;
    [SerializeField] TMP_Text attackText;
    [SerializeField] Sprite blockSprite;

    // Miscellaneous variables.
    [HideInInspector] public bool accelerate = false;
    [HideInInspector] public bool isAttacking = false;
    [HideInInspector] public bool isLeft = true;
    [HideInInspector] public bool retreated = false;
    [HideInInspector] public bool isArtifact;
    [HideInInspector] public Artifact hasArtifact;
    [HideInInspector] public int artifactUsed = -1;
    [HideInInspector] public Vector3 oldPos, newPos;
    [HideInInspector] public int health, attack;
    [HideInInspector] public bool fast;
    [HideInInspector] public int agile, block, armor;
    [HideInInspector] public bool antiAgile, antiBlock;
    [HideInInspector] public bool morph, combo, hasCombo = false;
    [HideInInspector] public bool slow, steady;
    [HideInInspector] public float moveTime;
    [HideInInspector] public List<Ability> abilities = new List<Ability>();

    float progress;
    Sprite healthSprite;
    //animFactor;
    //float spriteFrame = 0f;
    Ability newAbility;

    //Start is called before the first frame update.
    new void Start()
    {
        base.Start();
        healthSprite = healthImage.sprite;

        if (isArtifact)
        {
            if (hasArtifact)
                AddAbility(hasArtifact.ability);
            newPos = transform.position;
            health = 1;
        }
        else
        {
            SetMoveAnimation(true);
            foreach (Ability a in unit.abilities)
                AddAbility(a);
        }

        fast = unit.fast;
        agile = unit.agile;
        block = unit.block;
        armor = unit.armor;
        antiAgile = unit.antiAgile && !isArtifact;
        antiBlock = unit.antiBlock && !isArtifact;
        morph = unit.morph && !isArtifact;
        combo = unit.combo;
        slow = unit.slow;
        steady = unit.steady && !isArtifact;
        moveTime = 0;
        oldPos = transform.position;
    }

    //Update is called once per frame.
    new void Update()
    {
        base.Update();

        healthImage.sprite = block > 0 ? blockSprite : healthSprite;
        healthText.text = block > 0 ? block.ToString() : health.ToString();
        attackText.text = attack.ToString();

        // Movement
        if (moveTime < 0.5f)
        {
            moveTime += Time.deltaTime *
            (useBattleSpeed
            ? Master.data.battleSpeed
            : 1) / 2;
            progress = Master.AnimationCurve
                (moveTime * 2,
                accelerate,
                !accelerate);
            transform.position = Vector3.Lerp
                (oldPos,
                newPos,
                progress);
        }
        // Idle
        else
        {
            transform.position = newPos;
            if (animator.GetBool("moving"))
                SetMoveAnimation(false);
        }
    }

    void OnDestroy()
    {
        if (forCombat)
            foreach (Ability a in abilities)
                Battle
                    .fighterAbilities[a.cause.type][a.effects[0].type]
                    .Remove(a);
    }

    public void AddAbility(Ability a)
    {
        newAbility = new Ability();
        newAbility.owner = this;
        newAbility.description = a.description;
        newAbility.cause = a.cause;
        newAbility.effects = new List<Effect>();
        foreach (Effect effect in a.effects)
            newAbility.effects.Add(effect);
        abilities.Add(newAbility);

        if (forCombat)
            Battle
                .fighterAbilities[a.cause.type][a.effects[0].type]
                .Add(newAbility);
    }

    public void SetAttackAnimation(bool attack)
    {
        SetAnimation(attack ? unit.attackId : -1);
    }

    public void SetAbilityAnimation(bool ability)
    {
        SetAnimation(ability ? unit.abilityId : -1);
    }

    public void SwitchSides()
    {
        /// <summary>Switch sides in battle.</summary>

        // Transform
        isLeft = !isLeft;
        if (isLeft)
        {
            transform.localScale = new Vector3(-1, 1, 1) * offsetScale;
            statCanvas.localScale = new Vector3(-1, 1, 1) * offsetScale * 0.01f;
        }
        else
        {
            transform.localScale = Vector3.one * offsetScale;
            statCanvas.localScale = Vector3.one * offsetScale * 0.01f;
        }
        transform.localScale *= unit.bodySize;

        // Health/Attack display
        Vector3 temp = healthText.transform.parent.position;
        healthText.transform.parent.position = attackText.transform.parent.position;
        attackText.transform.parent.position = temp;
    }

    public void NewPos
        (Vector3 pos,
        bool animate = true,
        bool charge = false)
    {
        /// <summary>Initiate movement toward a new position.</summary>
        /// <param name="pos">The new position to move toward.</param>
        /// <param name="animate">If set to false, the fighter will not use its move animation.</param>
        /// <param name="charge">Whether or not the fighter is charging into battle.</param>

        oldPos = transform.position;
        newPos = pos;
        moveTime = 0;
        accelerate = charge;

        // Decide whether or not to switch to the move sprite state.
        if
            (animate
            && !animator.GetBool("dead")
            && ((newPos - oldPos).magnitude >= 0.5f))
            SetMoveAnimation(true);
    }
}