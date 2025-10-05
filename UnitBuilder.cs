using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBuilder : MonoBehaviour
{
    /// <summary>
    /// Used in editor mode to set up a unit's appearance after designing it in the SPUM editor.
    /// </summary>

    public SPUM_SpriteList spriteList;
    public SPUM_HorseSpriteList horseList;
    public Unit unit;

    // Start is called before the first frame update
    void Start()
    {
        UpdateList(spriteList._itemList, unit.appearance.itemSet);
        UpdateList(spriteList._eyeList, unit.appearance.eyeSet);
        UpdateList(spriteList._hairList, unit.appearance.hairSet);
        UpdateList(spriteList._bodyList, unit.appearance.bodySet);
        UpdateList(spriteList._clothList, unit.appearance.clothSet);
        UpdateList(spriteList._armorList, unit.appearance.armorSet);
        UpdateList(spriteList._pantList, unit.appearance.pantSet);
        UpdateList(spriteList._weaponList, unit.appearance.weaponSet);
        UpdateList(spriteList._backList, unit.appearance.backSet);

        if (horseList)
            UpdateList(horseList._spList, unit.appearance.horseSet);
    }

    void UpdateList(List<SpriteRenderer> reference, List<Appearance.SpriteSet> list)
    {
        foreach (SpriteRenderer renderer in reference)
        {
            Appearance.SpriteSet set = new Appearance.SpriteSet();
            set.sprite = renderer.sprite;
            set.color = renderer.color;
            list.Add(set);
        }
    }
}
