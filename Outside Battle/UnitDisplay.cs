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

    // Miscellaneous variables.
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
            (!useBattleSpeed || (!animator.GetBool("dead") && animator.GetInteger("misc") < 0)
            ? animSpeed
            : Master.data.battleSpeed);
    }

    public void SetAnimation(int id = -1)
    {
        animator.SetInteger("misc", id);
    }
    public void SetMoveAnimation(bool move = false)
    {
        animator.SetBool("moving", move);
        if (move)
            SetAnimation();
    }
    public void Die(bool isDead = true)
    {
        SetAnimation();
        SetMoveAnimation();
        animator.SetBool("dead", isDead);
    }

    void UpdateSpriteSet
        (List<Appearance.SpriteSet> reference,
        List<SpriteRenderer> list)
    {
        for
            (int s = 0;
            s < Mathf.Min(reference.Count, list.Count);
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
            model = Instantiate(unit.appearance.model, transform);
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
        {
            UpdateSpriteSet(unit.appearance.itemSet, sprites._itemList);
            UpdateSpriteSet(unit.appearance.eyeSet, sprites._eyeList);
            UpdateSpriteSet(unit.appearance.hairSet, sprites._hairList);
            UpdateSpriteSet(unit.appearance.bodySet, sprites._bodyList);
            UpdateSpriteSet(unit.appearance.clothSet, sprites._clothList);
            UpdateSpriteSet(unit.appearance.armorSet, sprites._armorList);
            UpdateSpriteSet(unit.appearance.pantSet, sprites._pantList);
            UpdateSpriteSet(unit.appearance.weaponSet, sprites._weaponList);
            UpdateSpriteSet(unit.appearance.backSet, sprites._backList);

            if (horseSprites)
                UpdateSpriteSet(unit.appearance.horseSet, horseSprites._spList);
        }
    }
}