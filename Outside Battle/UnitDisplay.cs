using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitDisplay : MonoBehaviour
{
    /// <summary>
    /// Displays and animates a unit.
    /// </summary>

    // Serialized variables for the editor.
    public Unit unit;
    public bool canBeBig;
    public bool useBattleSpeed;
    [Tooltip("Leave 0 for a random speed.")] public float animSpeed;
    public float offsetScale = 1;

    public enum AnimState
    {
        Idle,
        Move,
        Attack,
        Ability,
        Die
    }

    // Miscellaneous variables.
    [HideInInspector] public AnimState state = AnimState.Idle;
    [HideInInspector] public Animator animator;
    [HideInInspector] public SPUM_SpriteList sprites;
    [HideInInspector] public SPUM_HorseSpriteList horseSprites;
    [HideInInspector] public Colour colour;

    GameObject model;

    //Start is called before the first frame update.
    protected void Start()
    {
        if (animSpeed <= 0)
            animSpeed = Random.value / 2 + 0.75f;
    }

    //Update is called once per frame.
    protected void Update()
    {
        // Update the animator speed.
        animator.speed = 0.5f *
            (!useBattleSpeed || state == AnimState.Idle
            ? animSpeed
            : Master.data.battleSpeed);
    }

    public void SetAnimation(AnimState newState = AnimState.Idle)
    {
        if (state != AnimState.Die)
        {
            state = newState;
            switch (state)
            {
                case AnimState.Idle:
                    animator.Play("Idle");
                    break;
                case AnimState.Move:
                    animator.Play("Move");
                    break;
                case AnimState.Attack:
                    animator.Play(unit.attackString);
                    break;
                case AnimState.Ability:
                    animator.Play(unit.abilityString);
                    break;
                case AnimState.Die:
                    animator.Play("Die");
                    break;
                default:
                    Debug.LogWarning($"Unknown animation state: {state}");
                    break;
            }
        }
    }

    void UpdateSpriteSet
        (List<SpriteSet> reference,
        SpriteRenderer[] list)
    {
        for
            (int s = 0;
            s < Mathf.Min(reference.Count, list.Length);
            s++)
        {
            if (list[s] != null)
            {
                list[s].sprite = reference[s].sprite;
                list[s].color = colour.renderColour
                    ? Color.Lerp
                        (reference[s].color,
                        colour.physicalColour,
                        0.5f)
                    : reference[s].color;
            }
        }
    }

    public void ChangeUnit
        (Unit newUnit,
        string newColour = null,
        bool canBeBig = false)
    {
        /// <summary>Change the unit when necessary.</summary>
        /// <param name="newUnit">The new unit to change into.</param>
        /// <param name="newColour">The new colour to change into; null if the color doesn't change.</param>

        if (newUnit)
        {
            unit = newUnit;
            if (model)
                Destroy(model);
            model = Instantiate(unit.model, transform);
            animator = model.GetComponent<Animator>();
            sprites = model.transform.GetComponentInChildren<SPUM_SpriteList>();
            horseSprites = model.transform.GetComponentInChildren<SPUM_HorseSpriteList>();
        }

        if (newColour != null)
            colour = Master.colours[newColour];

        if (canBeBig)
        {
            float newX = Mathf.Sign(transform.localScale.x);
            Vector3 scale = new Vector3(newX, 1, 1);
            scale *= unit.bodySize * offsetScale;
            transform.localScale = scale;
        }

        if (unit != null)
            UpdateSpriteSet(unit.bodySet,
                model.GetComponentsInChildren<SpriteRenderer>());
    }
}